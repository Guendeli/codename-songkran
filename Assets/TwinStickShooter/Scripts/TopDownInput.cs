namespace TwinStickShooter
{
  using Photon.Deterministic;
  using Quantum;
  using UnityEngine;
  using UnityEngine.InputSystem;

  public class TopDownInput : MonoBehaviour
  {
    public FP AimSensitivity = 5;
    public CustomViewContext ViewContext;
    
    private FPVector2 _lastDirection = new FPVector2();
    private AttackPreview _attackPreview;
    private PlayerInput _playerInput;

    public bool IsInverseControl { get; set; } = false;
    private bool _isUsingGamepad;
    
    #region Input Names
    
    public const string BUTTON_FIRE = "Fire";
    public const string BUTTON_SPECIAL = "Special";
    public const string BUTTON_MOUSE_FIRE = "MouseFire";
    public const string BUTTON_MOUSE_SPECIAL = "MouseSpecial";
    
    public const string VECTOR_MOVE = "Move";
    public const string VECTOR_AIM_BASIC = "AimBasic";
    public const string VECTOR_AIM_SPECIAL = "AimSpecial";
    public const string VECTOR_POINT = "Point";
    
    #endregion
    
    private void Start()
    {
      _playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
      CharacterView.OnLocalPlayerInstantiated += OnLocalPlayerInstantiated;
      QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
    }

    private void OnDisable()
    {
      CharacterView.OnLocalPlayerInstantiated -= OnLocalPlayerInstantiated;
    }


    private void OnLocalPlayerInstantiated(CharacterView playerView)
    {
      if (_attackPreview != null)
      {
        Destroy(_attackPreview.gameObject);
      }
      _attackPreview = ViewContext.LocalView.GetComponentInChildren<AttackPreview>(true);
    }

    private void Update()
    {
      if (GamepadIsActuated() && !_isUsingGamepad)
      {
        _isUsingGamepad = true;
      } 
      
      if(MouseIsActuated() && _isUsingGamepad)
      {
        _isUsingGamepad = false;
      }

      bool noAttackPressed = (_playerInput.actions[BUTTON_FIRE].IsPressed() == false
                              && _playerInput.actions[BUTTON_SPECIAL].IsPressed() == false)
                             && (_playerInput.actions[BUTTON_MOUSE_FIRE].IsPressed() == false
                                 && _playerInput.actions[BUTTON_MOUSE_SPECIAL].IsPressed() == false);
      
      bool isAiming = _isUsingGamepad == false || (_isUsingGamepad  &&
                                                   (_playerInput.actions[VECTOR_AIM_BASIC].ReadValue<Vector2>() != Vector2.zero 
                                                    || _playerInput.actions[VECTOR_AIM_SPECIAL].ReadValue<Vector2>() != Vector2.zero));
      
      if (_attackPreview != null && noAttackPressed)
      {
          _attackPreview.gameObject.SetActive(false);  
      }
    }

    public void PollInput(CallbackPollInput callback)
    {
      Quantum.QuantumDemoInputTopDown input = new Quantum.QuantumDemoInputTopDown();

      FPVector2 directional = _playerInput.actions[VECTOR_MOVE].ReadValue<Vector2>().ToFPVector2();
      input.MoveDirection = IsInverseControl == true ? -directional : directional;

#if UNITY_ANDROID
		input.Fire = _playerInput.actions["Fire"].IsPressed();
		input.AltFire = _playerInput.actions["Special"].IsPressed();
#endif
#if UNITY_STANDALONE || UNITY_WEBGL
      input.Fire = _playerInput.actions[BUTTON_MOUSE_FIRE].IsPressed() || _playerInput.actions[BUTTON_FIRE].IsPressed();
      input.AltFire = _playerInput.actions[BUTTON_MOUSE_SPECIAL].IsPressed() || _playerInput.actions[BUTTON_SPECIAL].IsPressed();
#endif

      if (input.Fire == true)
      {
        Vector2 tempDir = _playerInput.actions[VECTOR_AIM_BASIC].ReadValue<Vector2>();
        if (!Mathf.Approximately(tempDir.sqrMagnitude, 0f))
        {
          _lastDirection = tempDir.ToFPVector2();
          _lastDirection *= AimSensitivity;
        }
        
      }

      if (input.AltFire == true)
      {
        Vector2 tempDir = _playerInput.actions[VECTOR_AIM_SPECIAL].ReadValue<Vector2>();
        if (!Mathf.Approximately(tempDir.sqrMagnitude, 0f))
        {
          _lastDirection = tempDir.ToFPVector2();
          _lastDirection *= AimSensitivity;
        }
      }

      FPVector2 actionVector = default;
      if (_isUsingGamepad)
      {
        actionVector = IsInverseControl ? -_lastDirection : _lastDirection;
        input.AimDirection = actionVector;
      }
      else
      {
        actionVector = GetDirectionToMouse();
        input.AimDirection = actionVector;
      }

      if ((input.Fire == true || input.AltFire == true))
      {
        _attackPreview.gameObject.SetActive(true);
        _attackPreview.UpdateAttackPreview(actionVector, input.AltFire);
      }

      callback.SetInput(input, DeterministicInputFlags.Repeatable);
      
    }

    private FPVector2 GetDirectionToMouse()
    {
      if (QuantumRunner.Default == null || QuantumRunner.Default.Game == null)
        return default;

      Frame frame = QuantumRunner.Default.Game.Frames.Predicted;
      if (frame == null)
        return default;

      if (ViewContext.LocalView == null || frame.Exists(ViewContext.LocalView.EntityRef) == false)
        return default;
      

      FPVector2 localCharacterPosition = frame.Get<Transform2D>(ViewContext.LocalView.EntityRef).Position;

      Vector2 mousePosition = _playerInput.actions[VECTOR_POINT].ReadValue<Vector2>();
      Ray ray = Camera.main.ScreenPointToRay(mousePosition);
      UnityEngine.Plane plane = new UnityEngine.Plane(Vector3.up, Vector3.zero);

      if (plane.Raycast(ray, out var enter))
      {
        var dirToMouse = ray.GetPoint(enter).ToFPVector2() - localCharacterPosition;
        return dirToMouse;
      }

      return default;
    }
    
    private bool GamepadIsActuated()
    {
      if (Gamepad.current == null)
        return false;
      
      return Gamepad.current.wasUpdatedThisFrame;
    }

    private bool MouseIsActuated()
    {
      bool isMouse = false;
      if (Mouse.current != null)
      {
        isMouse = Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.middleButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame;
        isMouse |= Mouse.current.delta.value != Vector2.zero;
      }

      return (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame);
    }
  }
}
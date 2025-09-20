namespace TwinStickShooter
{
  using UnityEngine;

  public class AttackColorSetter : ColorSetter
  {
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private int _targetMaterialId;
    [SerializeField] private Quantum.EntityView ViewAsset;

    public override long ColorId => ViewAsset.GetInstanceID();

    public override void SetLocal()
    {
      if (_meshRenderer == null) return;
      
      _meshRenderer.materials[_targetMaterialId].color = GetLocalColor(ColorId);
    }

    public override void SetRemote(bool isFriendly)
    {
      if (_meshRenderer == null) return;

      _meshRenderer.materials[_targetMaterialId].color = GetRemoteColor(ColorId, isFriendly);
    }
  }
}
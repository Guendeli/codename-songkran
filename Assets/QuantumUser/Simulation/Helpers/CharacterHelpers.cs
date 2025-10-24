namespace Quantum
{
    public static unsafe class CharacterHelpers
    {
        public static bool CanAutoAim(Frame frame, EntityRef source, SkillData skillData, InputContainer* inputContainer)
        {
            Bot* bot = frame.Unsafe.GetPointer<Bot>(source);
            if (bot->IsActive)
                return false;

            if (skillData.AutoAimCheckSight && inputContainer->TimeSinceAutoAim <= 0)
            {
                return true;
            }
      
            return false;
        }
        
        public static bool IsBot(Frame frame, EntityRef source)
        {
            if (frame.TryGet<Bot>(source, out Bot bot))
            {
                return bot.IsActive;
            }

            return false;
        }
    }
}
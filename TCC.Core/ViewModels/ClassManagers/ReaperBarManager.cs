﻿using TCC.Data;
using TCC.Data.Databases;

namespace TCC.ViewModels
{
    public class ReaperBarManager : ClassManager
    {

        public DurationCooldownIndicator ShadowReaping { get; set; }
        public ReaperBarManager() : base()
        {
        }
        public override void LoadSpecialSkills()
        {
            ShadowReaping = new DurationCooldownIndicator(_dispatcher);
            SessionManager.SkillsDatabase.TryGetSkill(160100, Class.Reaper, out var sr);
            ShadowReaping.Cooldown = new FixedSkillCooldown(sr, true);
            ShadowReaping.Buff= new FixedSkillCooldown(sr, true);
        }

        public override bool StartSpecialSkill(SkillCooldown sk)
        {
            if (sk.Skill.IconName == ShadowReaping.Cooldown.Skill.IconName)
            {
                ShadowReaping.Cooldown.Start(sk.Cooldown);
                return true;
            }
            return false;
        }
    }
}

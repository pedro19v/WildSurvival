﻿public class Tent : SimpleBuilding
{
    protected override void OnStart()
    {
        base.OnStart();
        Upgrade();
    }

    protected override void OnUpgrade()
    {
        // Maybe increase players' rested xp or tent's HP
    }
}

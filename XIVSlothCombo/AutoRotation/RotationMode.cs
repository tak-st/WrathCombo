﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVSlothCombo.AutoRotation
{
    public enum DPSRotationMode
    {
        Manual,
        Highest_Max,
        Lowest_Max,
        Highest_Current,
        Lowest_Current,
        Tank_Target,
        Nearest,
        Furthest,
    }

    public enum HealerRotationMode
    {
        Manual,
        Highest_Current,
        Lowest_Current,
        //Self_Priority,
        //Tank_Priority,
        //Healer_Priority,
        //DPS_Priority,
    }
}

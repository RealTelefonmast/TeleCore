﻿using System;

namespace TeleCore;

[Flags]
public enum NetworkIOMode : byte
{
    Input = 1,                  //0001
    Output = 2,                 //0010
    None = 4,                   //0100
    TwoWay = Input & Output     //0011
}
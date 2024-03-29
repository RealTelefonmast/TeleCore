﻿using UnityEngine;
using Verse;
using Verse.Noise;

namespace TeleCore;

public class OutputNode : ModuleNode
{
    public OutputNode(Vector2 position) : base(position)
    {
        Window_ModuleVisualizer.Vis.Notify_NewOutput(this);
    }

    protected override int Inputs => 1;
    protected override int Outputs => 0;
    public override string ModuleName => "Output";

    public ModuleBase FinalBase
    {
        get
        {
            if (IOAnchors.InputPanel.HasAnchoredNode) return IOAnchors.InputPanel.AnchoredNodes[0]?.ModuleData.Module;

            return null;
        }
    }

    protected override ModuleBase CreateModule()
    {
        return null;
    }
}

//
public class RidgedMultifractalNode : ModuleNode
{
    public RidgedMultifractalNode(Vector2 position) : base(position)
    {
    }

    public override string ModuleName => "RidgedMultifractal";
    protected override int Inputs => 0;
    protected override int Outputs => 1;

    protected override ModuleBase CreateModule()
    {
        return new RidgedMultifractal(0.035, 2.0, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
    }
}

public class PerlinNode : ModuleNode
{
    public PerlinNode(Vector2 position) : base(position)
    {
    }

    public override string ModuleName => "Perlin";
    protected override int Inputs => 0;
    protected override int Outputs => 1;

    protected override ModuleBase CreateModule()
    {
        return new Perlin(0.035f, 2.0, 0.40000000596046448, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
    }
}

public class TurbulenceNode : ModuleNode
{
    public TurbulenceNode(Vector2 position) : base(position)
    {
    }

    public override string ModuleName => "Turbulence";
    protected override int Inputs => 1;
    protected override int Outputs => 1;

    protected override ModuleBase CreateModule()
    {
        return new Turbulence(null);
    }
}

public class TerraceNode : ModuleNode
{
    public TerraceNode(Vector2 position) : base(position)
    {
    }

    public override string ModuleName => "Terrace";
    protected override int Inputs => 1;
    protected override int Outputs => 1;

    protected override ModuleBase CreateModule()
    {
        return new Terrace(false, null);
    }
}

//VALUES
public class ConstNode : ModuleNode
{
    public ConstNode(Vector2 position) : base(position)
    {
    }

    public override string ModuleName => "Const";
    protected override int Inputs => 0;
    protected override int Outputs => 1;

    protected override ModuleBase CreateModule()
    {
        return new Const(0.5f);
    }
}

//MATH
public class AddNode : ModuleNode
{
    public AddNode(Vector2 position) : base(position)
    {
    }

    public override string ModuleName => "Add";
    protected override int Inputs => 2;
    protected override int Outputs => 1;

    protected override ModuleBase CreateModule()
    {
        return new Add(null, null);
    }
}

public class SubtractNode : ModuleNode
{
    public SubtractNode(Vector2 position) : base(position)
    {
    }

    public override string ModuleName => "Subtract";
    protected override int Inputs => 2;
    protected override int Outputs => 1;

    protected override ModuleBase CreateModule()
    {
        return new Subtract(null, null);
    }
}

public class MultiplyNode : ModuleNode
{
    public MultiplyNode(Vector2 position) : base(position)
    {
    }

    public override string ModuleName => "Multiply";
    protected override int Inputs => 2;
    protected override int Outputs => 1;

    protected override ModuleBase CreateModule()
    {
        return new Multiply(null, null);
    }
}

public class ExponentNode : ModuleNode
{
    public ExponentNode(Vector2 position) : base(position)
    {
    }

    public override string ModuleName => "Exponent";
    protected override int Inputs => 1;
    protected override int Outputs => 1;

    protected override ModuleBase CreateModule()
    {
        return new Exponent(1, null);
    }
}

public class PowerNode : ModuleNode
{
    public PowerNode(Vector2 position) : base(position)
    {
    }

    public override string ModuleName => "Power";
    protected override int Inputs => 2;
    protected override int Outputs => 1;

    protected override ModuleBase CreateModule()
    {
        return new Power(null, null);
    }
}

//PROCESSORS
public class RotateNode : ModuleNode
{
    public RotateNode(Vector2 position) : base(position)
    {
    }

    public override string ModuleName => "Rotate";
    protected override int Inputs => 1;
    protected override int Outputs => 1;

    protected override ModuleBase CreateModule()
    {
        return new Rotate(1, 1, 1, null);
    }
}

public class SelectNode : ModuleNode
{
    public SelectNode(Vector2 position) : base(position)
    {
    }

    public override string ModuleName => "Select";
    protected override int Inputs => 3;
    protected override int Outputs => 1;

    protected override ModuleBase CreateModule()
    {
        return new Select(null, null, null);
    }
}

public class TranslateNode : ModuleNode
{
    public TranslateNode(Vector2 position) : base(position)
    {
    }

    public override string ModuleName => "Translate";
    protected override int Inputs => 1;
    protected override int Outputs => 1;

    protected override ModuleBase CreateModule()
    {
        return new Translate(null);
    }
}

public class ClampNode : ModuleNode
{
    public ClampNode(Vector2 position) : base(position)
    {
    }

    public override string ModuleName => "Abs";
    protected override int Inputs => 1;
    protected override int Outputs => 1;

    protected override ModuleBase CreateModule()
    {
        return new Clamp(0, 100, null);
    }
}

public class AbsNode : ModuleNode
{
    public AbsNode(Vector2 position) : base(position)
    {
    }

    public override string ModuleName => "Abs";
    protected override int Inputs => 1;
    protected override int Outputs => 1;

    protected override ModuleBase CreateModule()
    {
        return new Abs(null);
    }
}

public class BlendNode : ModuleNode
{
    public BlendNode(Vector2 position) : base(position)
    {
    }

    public override string ModuleName => "Blend";
    protected override int Inputs => 3;
    protected override int Outputs => 1;

    protected override ModuleBase CreateModule()
    {
        return new Blend(null, null, null);
    }
}
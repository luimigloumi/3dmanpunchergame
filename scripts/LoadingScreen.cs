using Godot;
using System;

public partial class LoadingScreen : Node
{

    [Export] public NodePath barPath;
    public ProgressBar bar;

	Godot.Collections.Array progress = new();

	public string scenePath = "";

	Node loadingScreen;
	
	public override void _Ready() {

        scenePath = (string)((KeepVariables)GetNode("/root/KeepVariables")).variables["loadScenePath"];
        ((KeepVariables)GetNode("/root/KeepVariables")).variables.Remove("loadScenePath");

        bar = GetNode<ProgressBar>(barPath);

		ResourceLoader.LoadThreadedRequest(scenePath);

	}

    public override void _Process(double delta)
    {
        
		if (ResourceLoader.LoadThreadedGetStatus(scenePath, progress) == ResourceLoader.ThreadLoadStatus.Loaded) {

			if (loadingScreen != null && loadingScreen.IsInsideTree()) loadingScreen.QueueFree();
			PackedScene newScene = (PackedScene)ResourceLoader.LoadThreadedGet(scenePath);
            GetTree().ChangeSceneToPacked(newScene);
			
		} else {

            bar.Value = Mathf.Lerp(bar.Value, (float)progress[0] * 100, 0.25f);

		}

    }

}

using Godot;
using System;

public partial class LoadSceneButton : Button
{

	public void OnPressed() {

		if (scenePath == "Quit") {

			GetTree().Quit();

		} else {

			((KeepVariables)GetNode("/root/KeepVariables")).variables.Add("loadScenePath", scenePath);
			GetTree().ChangeSceneToPacked(loadingScene);

		}

	}

	[Export(PropertyHint.File, "*.tscn")] public string scenePath;
	[Export] public PackedScene loadingScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		Input.MouseMode = Input.MouseModeEnum.Visible;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		

	}

}

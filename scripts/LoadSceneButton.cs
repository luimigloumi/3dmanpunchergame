using Godot;
using System;

public partial class LoadSceneButton : Button
{

	[Export] public bool showLoadingScene = true;

	public void OnPressed() {

		if (scenePath == "Quit") {

			GetTree().Quit();

		} else {

			if (showLoadingScene) {
				
				((KeepVariables)GetNode("/root/KeepVariables")).variables.Add("loadScenePath", scenePath);
				GetTree().ChangeSceneToPacked(loadingScene);

			} else {

				GetTree().ChangeSceneToFile(scenePath);

			}

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

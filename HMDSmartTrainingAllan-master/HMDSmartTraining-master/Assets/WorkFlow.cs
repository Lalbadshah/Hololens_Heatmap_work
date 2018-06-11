using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is a c# representation of our jsonInstructions.txt template file

[System.Serializable]
public class WorkFlow
{
	public JuxtopiaTask JuxtopiaTask;
}

[System.Serializable]
public class JuxtopiaTask
{
	public double Version;
	public string Name;
    public string ResourcesPath;
	public List<Step> Steps;
}

[System.Serializable]
public class Step
{
	public string Name;
	public double Version;
	public int Duration;
	public List<FeatureAnchoredObject> FeatureAnchoredObjects;
	public List<DisplayAnchoredImage> DisplayAnchoredImage;
	public List<DisplayAnchoredText> DisplayAnchoredText;
	public Wheel Wheel;
}

[System.Serializable]
public class FeatureAnchoredObject
{
	public string Image_Path;
	public List<double> Position;
	public string MarkerName;
	public List<AnimationPath> AnimationPath;
}

[System.Serializable]
public class DisplayAnchoredImage
{
	public string Image_Path;
	public List<double> Position;
	public float Scale;
	public List<AnimationPath> AnimationPath;

}

[System.Serializable]
public class AnimationPath
{
	public List<double> Position;
	public double Time;
}


[System.Serializable]
public class DisplayAnchoredText
{
	public string Text;
	public List<double> Position;
	public float Scale;
}

[System.Serializable]
public class Wheel
{
	public string Text;
	public string Icon;
}

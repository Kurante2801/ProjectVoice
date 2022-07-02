using System;
using System.Collections.Generic;

[Serializable]
public class LevelMeta
{
	public string id;
	public string title;
	public string title_localized;
	public string artist;
	public string artist_localized;
	public string artist_source;
	public string illustrator;
	public string illustrator_source;
	public string illustrator_localized;
	public string charter;

	public string music_path;
	public int preview_time;
	public string preview_path;
	public string background_path;
	public float? background_aspect_ratio;

	public List<ChartSection> charts = new();
}

[Serializable]
public class ChartSection
{
	public string path;
	public int difficulty;
	public string name;
	public DifficultyType type;
	public string music_override;
	public float[] bpms;
}

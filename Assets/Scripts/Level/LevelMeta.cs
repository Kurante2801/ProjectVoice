[System.Serializable]
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
	public string charter;
	public int preview_time;

	public string music_path;
	public string preview_path;
	public string background_path;
}

[System.Serializable]
public class ChartSection
{
	public string path;
	public int difficulty;
	public string name;
	public string type;
	public string music_override;
}

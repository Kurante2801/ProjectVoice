using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class ColorExtensions
{
	/// <summary>
	/// Returns a HEX string
	/// </summary>
	/// <param name="color"></param>
	/// <param name="hashtag"></param>
	/// <returns></returns>
	public static string ToHEX(this Color color, bool hashtag = false, bool alpha = false) => ((Color32)color).ToHEX(hashtag, alpha);
   
	/// <summary>
	/// Returns a HEX string
	/// </summary>
	/// <param name="color"></param>
	/// <param name="hashtag"></param>
	/// <returns></returns>
	public static string ToHEX(this Color32 color, bool hashtag = false, bool alpha = false)
	{
		string hex = hashtag ? "#" : "";
		hex += color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        if (alpha)
            hex += color.a.ToString("X2");

        return  hex;
	}

	private static readonly char[] ValidHEX =
	{
		'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
		'A', 'B', 'C', 'D', 'E', 'F'
	};
	/// <summary>
	/// Returns a string that will always be a valid HEX color
	/// </summary>
	/// <param name="hex"></param>
	/// <returns></returns>
	public static string ValidateHEX(this string hex, bool parseAlpha = false)
	{
		hex = hex.Replace("#", "").ToUpper() + "00000000";
        int length = parseAlpha ? 8 : 6;
        var builder = new StringBuilder(hex);

		for (int i = 0; i < length; i++)
		{
			if (!ValidHEX.Contains(hex[i]))
			{
				builder[i] = 'F';
			}
		}

		return builder.ToString(0, length);
	}
	/// <summary>
	/// Validates the string to a HEX value and returns a Color based on it
	/// </summary>
	/// <param name="hex"></param>
	/// <returns></returns>
	public static Color ToColor(this string hex, bool parseAlpha = false)
	{
		hex = ValidateHEX(hex);
		return new Color32(
			byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
			byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
			byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
			parseAlpha ? byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) : (byte)255
        );
	}

	public static Color WithAlpha(this Color color, float alpha)
	{
		return new Color(color.r, color.g, color.b, alpha);
	}
	public static Color32 WithAlpha(this Color32 color, byte alpha)
	{
		return new Color32(color.r, color.g, color.b, alpha);
	}
}

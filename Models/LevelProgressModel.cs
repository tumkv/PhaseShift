namespace PhaseShift.Models;

public class LevelProgressModel
{
    public bool IsFading = false;
    public bool RestartAfterFade = false;
    public float FadeAlpha = 0f;

    public const float FadeSpeed = 2.5f;

    public bool LevelCompletedScreen = false;
    public int CompletedLevelNumber = 1;
    public float LevelCompleteAlpha = 0f;

    public const float LevelCompleteFadeSpeed = 2f;
}
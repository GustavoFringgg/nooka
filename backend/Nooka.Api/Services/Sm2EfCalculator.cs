namespace Nooka.Api.Services;

public static class Sm2EfCalculator
{
    private const float MinEF = 1.3f; // TODO: 這裡沒有 static 是因為 const 就隱含 static
    private const float CorrectMultiplier = 1.05f;
    private const float IncorrectMultiplier = 0.85f;
    public static float ApplyAnswer(float currentEF, bool isCorrect)
    {
        var multiplier = isCorrect ? CorrectMultiplier : IncorrectMultiplier;
        return Math.Max(MinEF, currentEF * multiplier);
    }
}
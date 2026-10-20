using Nooka.Api.Services;

namespace Nooka.Api.Tests;

public class Sm2EfCalculatorTests
{
    [Fact] // 標記獨立測試方法,不帶參數
    public void ApplyAnswer_WhenCorrect_MultipliesBy1_05() // 被測方法_情境_預期結果
    {
        var result = Sm2EfCalculator.ApplyAnswer(2.5f, isCorrect: true);
        Assert.Equal(2.625f, result, precision: 3); // precision: 比到小數點後3位
    }

    [Fact]
    public void ApplyAnswer_WhenIncorrect_MultipliesBy0_85()
    {
        var result = Sm2EfCalculator.ApplyAnswer(2.5f, isCorrect: false);
        Assert.Equal(2.125f, result, precision: 3);
    }

    [Fact]
    public void ApplyAnswer_WhenAlreadyAtMinimum_ClampsToMinEF()
    {
        var result = Sm2EfCalculator.ApplyAnswer(1.3f, isCorrect: false);
        Assert.Equal(1.3f, result, precision: 3);
    }
}
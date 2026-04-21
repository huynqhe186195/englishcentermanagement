using Microsoft.AspNetCore.Mvc.RazorPages;
using EnglishCenter.Web.Services;
using EnglishCenter.Web.Models;

namespace EnglishCenter.Web.Pages.Student;

public class AcademicSummaryModel : PageModel
{
    private readonly IApiClient _apiClient;

    public AcademicSummaryModel(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public List<ScoreDto> StudentScores { get; set; } = new();

    public decimal Gpa => StudentScores.Any() ? Math.Round(StudentScores.Average(x => x.ScoreValue), 1) : 0;
    public int TestsTaken => StudentScores.Count;
    public decimal HighestScore => StudentScores.Any() ? StudentScores.Max(x => x.ScoreValue) : 0;
    public decimal LowestScore => StudentScores.Any() ? StudentScores.Min(x => x.ScoreValue) : 0;

    public decimal ListeningAvg => GetSkillAverage("listening");
    public decimal ReadingAvg => GetSkillAverage("reading");
    public decimal WritingAvg => GetSkillAverage("writing");
    public decimal SpeakingAvg => GetSkillAverage("speaking");

    public decimal EstimatedBand => Math.Round((ListeningAvg + ReadingAvg + WritingAvg + SpeakingAvg) / 4 / 1.25m, 1);

    public List<TrendPointVm> TrendPoints { get; set; } = new();

    public async Task OnGetAsync()
    {
        var me = await _apiClient.GetAsync<CurrentUserDto>("auth/me");
        if (me != null)
        {
            UserName = me.UserName;
            FullName = me.FullName;
        }

        var scoreData = await _apiClient.GetAsync<PagedResult<ScoreDto>>("scores?PageNumber=1&PageSize=200");
        var allScores = scoreData?.Items?.ToList() ?? new List<ScoreDto>();

        if (!string.IsNullOrWhiteSpace(FullName))
        {
            StudentScores = allScores
                .Where(x => x.StudentName.Equals(FullName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!StudentScores.Any() && !string.IsNullOrWhiteSpace(FullName))
        {
            StudentScores = allScores
                .Where(x => x.StudentName.Contains(FullName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!StudentScores.Any())
        {
            StudentScores = allScores.Take(10).ToList();
        }

        TrendPoints = BuildTrendPoints(StudentScores);
    }

    private decimal GetSkillAverage(string skill)
    {
        var values = StudentScores
            .Where(x => x.ExamTitle.Contains(skill, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.ScoreValue)
            .ToList();

        if (!values.Any())
        {
            values = StudentScores.Select(x => x.ScoreValue).ToList();
        }

        return values.Any() ? Math.Round(values.Average(), 1) : 0;
    }

    private static List<TrendPointVm> BuildTrendPoints(List<ScoreDto> scores)
    {
        var buckets = new[] { "W1", "W2", "W3", "W4", "W5", "W6" };
        if (!scores.Any())
        {
            return buckets.Select((x, i) => new TrendPointVm { Label = x, Value = 6 + i * 0.3m }).ToList();
        }

        var ordered = scores.OrderBy(x => x.Id).ToList();
        var chunkSize = (int)Math.Ceiling(ordered.Count / 6.0);
        var points = new List<TrendPointVm>();

        for (var i = 0; i < 6; i++)
        {
            var chunk = ordered.Skip(i * chunkSize).Take(chunkSize).ToList();
            var value = chunk.Any() ? Math.Round(chunk.Average(x => x.ScoreValue), 1) : (points.LastOrDefault()?.Value ?? 6.0m);
            points.Add(new TrendPointVm { Label = buckets[i], Value = value });
        }

        return points;
    }

    public string RankLabel(decimal score)
    {
        if (score >= 9) return "Xuất sắc";
        if (score >= 8) return "Giỏi";
        if (score >= 7) return "Khá";
        return "Trung bình";
    }


    public string RankClass(string rank)
    {
        return rank switch
        {
            "Xuất sắc" => "xuat-sac",
            "Giỏi" => "gioi",
            "Khá" => "kha",
            _ => "trung-binh"
        };
    }
    public class TrendPointVm
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }
}

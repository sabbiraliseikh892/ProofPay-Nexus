
namespace ProofPayNexus.Application.DTOs.PaymentAssessments;

public class PaymentRiskFactor
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsTriggered { get; set; }

    public decimal ScoreImpact { get; set; }

    public string Severity { get; set; } = string.Empty;
}

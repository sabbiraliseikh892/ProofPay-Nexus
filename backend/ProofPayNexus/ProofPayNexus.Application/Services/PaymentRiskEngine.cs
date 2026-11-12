using ProofPayNexus.Application.DTOs.PaymentAssessments;
using ProofPayNexus.Domain.Entities;
using ProofPayNexus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Services
{
    public class PaymentRiskEngine
    {
        public (decimal RiskScore, PaymentRecommendation Recommendation,
       List<PaymentRiskFactor> RiskFactors) CalculateRisk(
       List<Discrepancy> discrepancies)
        {
            if (discrepancies == null)
            {
                throw new ArgumentNullException(nameof(discrepancies));
            }

            var riskFactors = new List<PaymentRiskFactor>();

            decimal totalRiskScore = 0;
            bool hasCriticalDiscrepancy = false;

            var activeDiscrepancies = discrepancies
                .Where(x =>
                    x.Status == DiscrepancyStatus.Open ||
                    x.Status == DiscrepancyStatus.UnderReview)
                .ToList();

            foreach (var discrepancy in activeDiscrepancies)
            {
                decimal scoreImpact = 0;
                string severity = discrepancy.Severity.ToString();

                if (discrepancy.Severity == DiscrepancySeverity.Critical)
                {
                    scoreImpact = 30;
                    hasCriticalDiscrepancy = true;
                }
                else if (discrepancy.Severity == DiscrepancySeverity.High)
                {
                    scoreImpact = 20;
                }
                else if (discrepancy.Severity == DiscrepancySeverity.Medium)
                {
                    scoreImpact = 10;
                }
                else if (discrepancy.Severity == DiscrepancySeverity.Low)
                {
                    scoreImpact = 5;
                }

                totalRiskScore += scoreImpact;

                riskFactors.Add(new PaymentRiskFactor
                {
                    Code = discrepancy.Type.ToString(),
                    Name = discrepancy.Title,
                    Description = discrepancy.Description,
                    IsTriggered = true,
                    ScoreImpact = scoreImpact,
                    Severity = severity
                });
            }

            // Critical discrepancies must never receive an Approve recommendation.
            if (hasCriticalDiscrepancy && totalRiskScore < 60)
            {
                totalRiskScore = 60;
            }

            // Maximum risk score is 100.
            if (totalRiskScore > 100)
            {
                totalRiskScore = 100;
            }

            PaymentRecommendation recommendation;

            if (totalRiskScore <= 20)
            {
                recommendation = PaymentRecommendation.Approve;
            }
            else if (totalRiskScore < 60)
            {
                recommendation = PaymentRecommendation.Review;
            }
            else
            {
                recommendation = PaymentRecommendation.Dispute;
            }

            return (
                totalRiskScore,
                recommendation,
                riskFactors
            );
        }
    }
}

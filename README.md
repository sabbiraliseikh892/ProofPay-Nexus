# ProofPay Nexus

### AI-Powered Evidence-to-Payment Intelligence

**Tagline: Every payment should have proof.**

ProofPay Nexus is an AI-powered payment verification platform that helps businesses verify supplier invoices against purchase orders before authorizing payments.

It combines Gemini AI document extraction, deterministic invoice comparison, explainable payment risk assessment, human approval, and PayPal Sandbox payment authorization.

## The Problem

Businesses often process supplier payments using invoices and supporting documents without systematically verifying whether the invoice matches the original purchase order.

This can result in:

- Incorrect invoice amounts
- Supplier identity mismatches
- Missing or unexpected invoice items
- Duplicate or suspicious payment requests
- Financial losses caused by payment errors

## Our Solution

ProofPay Nexus introduces an evidence-first payment workflow.

Before payment authorization, the platform extracts invoice information, compares it with purchase order data, identifies discrepancies, and generates a risk assessment.

Payments with unacceptable risk are blocked from proceeding.

## Key Features

### 1. Purchase Order Management

- Create and manage purchase orders.
- Maintain supplier and item information.
- Track order amounts and delivery information.

### 2. AI-Powered Evidence Extraction

- Upload PDF, JPG and PNG documents.
- Use Google Gemini to extract invoice information.
- Extract invoice number, date, supplier, currency, totals and line items.
- Store structured extraction results.

### 3. Intelligent Invoice Comparison

- Compare invoice supplier with the purchase order supplier.
- Verify invoice totals.
- Compare purchased items, quantities and prices.
- Identify missing and unexpected items.

### 4. Explainable Discrepancy Detection

- Identify invoice mismatches.
- Classify discrepancy severity.
- Support human review and resolution.
- Maintain an audit trail.

### 5. Payment Risk Assessment

- Generate a deterministic risk score from 0 to 100.
- Identify risk factors.
- Provide Approve, Review or Dispute recommendations.
- Prevent payment when the recommendation is Dispute.

### 6. PayPal Integration

- Create PayPal Sandbox authorization orders.
- Redirect buyers to PayPal for approval.
- Authorize and capture eligible payments.
- Process verified PayPal webhooks.
- Maintain payment transaction records.

**Important:** PayPal authorization is not escrow. Human approval is required, and the platform does not claim that document analysis alone proves physical delivery or product authenticity.

## Technology Stack

| Layer             | Technology                     |
| ----------------- | ------------------------------ |
| Frontend          | React, TypeScript, Vite        |
| Backend           | ASP.NET Core 8 Web API         |
| Architecture      | Lightweight Clean Architecture |
| Database          | Microsoft SQL Server 2019      |
| ORM               | Entity Framework Core 8        |
| AI                | Google Gemini API              |
| AI SDK            | Google.GenAI                   |
| Payment           | PayPal REST API                |
| API Documentation | Swagger / OpenAPI              |
| Development       | Visual Studio 2022, VS Code    |

## System Workflow

1. Create a purchase order.
2. Upload the supplier invoice or supporting evidence.
3. Gemini extracts structured information.
4. The comparison engine verifies invoice information against the purchase order.
5. The system detects and stores discrepancies.
6. The payment risk engine calculates a risk score.
7. A human reviews the assessment.
8. Eligible payments proceed through PayPal authorization and capture.
9. Verified PayPal webhooks update transaction status.
10. Audit records preserve important workflow events.

## Architecture

```text
React + TypeScript Frontend
          |
          v
ASP.NET Core 8 Web API
          |
          +---- Application Layer
          |       |
          |       +---- Purchase Orders
          |       +---- Evidence Analysis
          |       +---- Invoice Comparison
          |       +---- Discrepancy Management
          |       +---- Payment Risk Engine
          |
          +---- Infrastructure Layer
          |       |
          |       +---- Google Gemini
          |       +---- Local File Storage
          |       +---- PayPal REST API
          |       +---- Webhook Processing
          |
          +---- Persistence Layer
                  |
                  +---- Entity Framework Core
                  +---- SQL Server 2019
```

## Project Structure

```text
ProofPay Nexus/
|
+-- ProofPayNexus.sln
|
+-- ProofPayNexus.API/
+-- ProofPayNexus.Application/
+-- ProofPayNexus.Domain/
+-- ProofPayNexus.Infrastructure/
+-- ProofPayNexus.Persistence/
+-- ProofPayNexus.Tests/
|
+-- frontend/
    +-- src/
    +-- package.json
    +-- vite.config.ts
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022
- Node.js and npm
- SQL Server 2019
- Google Gemini API key
- PayPal Developer Sandbox account

### Backend Configuration

Configure the database connection in the API development settings.

Configure sensitive credentials using .NET User Secrets rather than committing API keys to source control.

Required configuration sections:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_SQL_SERVER_CONNECTION_STRING"
  },
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY",
    "Model": "YOUR_CONFIGURED_GEMINI_MODEL"
  },
  "PayPal": {
    "BaseUrl": "https://api-m.sandbox.paypal.com",
    "ClientId": "YOUR_PAYPAL_CLIENT_ID",
    "ClientSecret": "YOUR_PAYPAL_CLIENT_SECRET",
    "WebhookId": "YOUR_PAYPAL_WEBHOOK_ID"
  }
}
```

Run the API from Visual Studio 2022.

Swagger documentation is available at:

`https://localhost:7040/swagger`

### Frontend Setup

Open a terminal in the frontend directory:

```bash
npm install
npm install axios lucide-react react-router-dom
npm run dev
```

Open:

`http://localhost:5173`

## Security and Payment Safety

- API credentials remain on the backend.
- Uploaded evidence is validated by file type and size.
- Documents are treated as untrusted input.
- AI extraction is not treated as proof of delivery.
- Payment risk is calculated using deterministic rules.
- Human approval is required before payment capture.
- PayPal webhook signatures are verified before processing events.
- Payment and audit information is persisted.

## Demonstrated MVP

The MVP demonstrates:

- Purchase order creation and retrieval.
- Evidence document upload.
- Gemini invoice extraction.
- Invoice-to-purchase-order comparison.
- Discrepancy detection.
- Payment risk assessment.
- PayPal Sandbox authorization and capture.
- Verified webhook processing.

## Demo

**Demo video:** Add your recorded demo URL here.

**Screenshots:** Add your project screenshots here.

## Future Improvements

- Multi-user authentication and role-based access.
- Advanced fraud detection.
- Duplicate invoice detection.
- More document formats.
- Cloud-based evidence storage.
- Production payment provider integration.
- Advanced supplier risk intelligence.

## Disclaimer

ProofPay Nexus is a hackathon MVP using PayPal Sandbox. It is not a production escrow service or a guarantee of physical delivery or product authenticity.

---

**ProofPay Nexus — Every payment should have proof.**

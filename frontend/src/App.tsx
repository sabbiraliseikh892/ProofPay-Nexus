import { useEffect, useState } from "react";
import {
  ShieldCheck,
  LayoutDashboard,
  ShoppingCart,
  FileText,
  AlertTriangle,
  CreditCard,
  Activity,
  Upload,
  RefreshCw,
} from "lucide-react";
import api from "./api";
import "./App.css";

interface PurchaseOrder {
  id: string;
  purchaseOrderNumber: string;
  supplierName: string;
  totalAmount: number;
  currencyCode: string;
  status: string | number;
}

interface Discrepancy {
  id: string;
  title: string;
  description: string;
  severity: string | number;
  status: string | number;
}

function App() {
  const [orders, setOrders] = useState<PurchaseOrder[]>([]);
  const [discrepancies, setDiscrepancies] = useState<Discrepancy[]>([]);
  const [selectedOrder, setSelectedOrder] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);

  const [evidenceId, setEvidenceId] = useState("");
  const [analysis, setAnalysis] = useState<any>(null);
  const [comparison, setComparison] = useState<any>(null);
  const [assessment, setAssessment] = useState<any>(null);

  // Load purchase orders
  const loadOrders = async () => {
    try {
      const response = await api.get("/PurchaseOrders");

      // API returns a direct JSON array
      const data = response.data;
      setOrders(Array.isArray(data) ? data : []);

      console.log("Purchase orders loaded:", data);
    } catch (error) {
      console.error("Failed to load purchase orders:", error);
      setOrders([]);
      setMessage("Unable to load purchase orders. Check API connection.");
    }
  };

  // Load discrepancies for selected purchase order
  const loadDiscrepancies = async (orderId: string) => {
    if (!orderId) {
      setDiscrepancies([]);
      return;
    }

    try {
      const response = await api.get(
        `/Discrepancies/purchase-order/${orderId}`,
      );

      const data = response.data;
      setDiscrepancies(Array.isArray(data) ? data : []);
    } catch (error) {
      console.error("Failed to load discrepancies:", error);
      setDiscrepancies([]);
      setMessage("Could not load discrepancies.");
    }
  };

  // Initial dashboard loading
  useEffect(() => {
    loadOrders();
  }, []);

  // Upload evidence document
  const uploadEvidence = async () => {
    if (!file || !selectedOrder) {
      setMessage("Select a purchase order and an evidence file.");
      return;
    }

    if (file.size > 10 * 1024 * 1024) {
      setMessage("File size must not exceed 10 MB.");
      return;
    }

    const form = new FormData();

    form.append("PurchaseOrderId", selectedOrder);
    form.append("File", file);
    form.append("FileName", file.name);
    form.append("ContentType", file.type);
    form.append("FileSizeInBytes", file.size.toString());
    form.append("DocumentType", "2");
    form.append("UploadedBy", "Admin");

    try {
      setLoading(true);
      setMessage("Uploading evidence...");

      const response = await api.post("/Evidence/upload", form);

      const uploaded = response.data?.data ?? response.data;
      const uploadedId = uploaded?.id ?? uploaded?.evidenceDocumentId;

      if (!uploadedId) {
        throw new Error("Upload succeeded, but evidence ID was not returned.");
      }

      setEvidenceId(uploadedId);
      setAnalysis(null);
      setComparison(null);
      setAssessment(null);

      setMessage("Evidence uploaded successfully.");
      setFile(null);
    } catch (error: any) {
      console.error("Evidence upload error:", error);

      setMessage(
        error.response?.data?.message ??
          error.message ??
          "Evidence upload failed.",
      );
    } finally {
      setLoading(false);
    }
  };

  // Gemini document analysis
  const analyzeEvidence = async () => {
    if (!evidenceId) {
      setMessage("Upload an evidence document first.");
      return;
    }

    try {
      setLoading(true);
      setMessage("Gemini is analysing your document...");

      const response = await api.post(
        `/EvidenceAnalysis/${evidenceId}/analyze`,
      );

      setAnalysis(response.data?.data ?? response.data);
      setMessage("AI evidence analysis completed.");
    } catch (error: any) {
      console.error("Evidence analysis error:", error);

      setMessage(
        error.response?.data?.message ??
          "AI analysis failed. Check Gemini configuration and API logs.",
      );
    } finally {
      setLoading(false);
    }
  };

  // Compare invoice against purchase order
  const compareInvoice = async () => {
    if (!evidenceId) {
      setMessage("Upload and analyse an invoice first.");
      return;
    }

    try {
      setLoading(true);
      setMessage("Comparing invoice with purchase order...");

      const response = await api.post(
        `/InvoiceComparison/${evidenceId}/compare`,
      );

      setComparison(response.data?.data ?? response.data);
      setMessage("Invoice comparison completed.");

      if (selectedOrder) {
        await loadDiscrepancies(selectedOrder);
      }
    } catch (error: any) {
      console.error("Invoice comparison error:", error);

      setMessage(error.response?.data?.message ?? "Invoice comparison failed.");
    } finally {
      setLoading(false);
    }
  };

  // Payment risk assessment
  const assessPayment = async () => {
    if (!selectedOrder) {
      setMessage("Select a purchase order first.");
      return;
    }

    try {
      setLoading(true);
      setMessage("Generating payment risk assessment...");

      const response = await api.post(
        `/PaymentAssessments/${selectedOrder}/assess`,
      );

      setAssessment(response.data?.data ?? response.data);
      setMessage("Payment risk assessment completed.");
    } catch (error: any) {
      console.error("Payment assessment error:", error);

      setMessage(error.response?.data?.message ?? "Payment assessment failed.");
    } finally {
      setLoading(false);
    }
  };

  // Create PayPal authorization order
  const createPayPalOrder = async () => {
    if (!selectedOrder) {
      setMessage("Select a purchase order first.");
      return;
    }

    try {
      setLoading(true);
      setMessage("Creating PayPal authorization order...");

      const response = await api.post("/PayPalPayment/create-order", {
        purchaseOrderId: selectedOrder,
      });

      const result = response.data?.data ?? response.data;
      const approvalUrl = result?.approvalUrl;

      if (approvalUrl) {
        window.location.href = approvalUrl;
      } else {
        setMessage("PayPal approval URL was not returned.");
      }
    } catch (error: any) {
      console.error("PayPal order error:", error);

      setMessage(
        error.response?.data?.message ??
          "PayPal order creation failed. Check payment assessment.",
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="app">
      <aside className="sidebar">
        <div className="brand">
          <ShieldCheck size={30} />
          <span>
            ProofPay<span className="brand-light"> Nexus</span>
          </span>
        </div>

        <div className="nav-label">WORKSPACE</div>

        <nav>
          <a className="active">
            <LayoutDashboard /> Dashboard
          </a>

          <a>
            <ShoppingCart /> Purchase Orders
          </a>

          <a>
            <FileText /> Evidence Management
          </a>

          <a>
            <AlertTriangle /> Discrepancies
          </a>

          <a>
            <CreditCard /> Payments
          </a>
        </nav>

        <div className="sidebar-bottom">
          <div className="secure">
            <ShieldCheck /> Secure payment workflow
          </div>

          <small>ProofPay Nexus · MVP</small>
        </div>
      </aside>

      <main className="main">
        <header className="topbar">
          <div>
            <h2>Payment Intelligence Dashboard</h2>
            <p>Evidence-driven payment verification</p>
          </div>

          <div className="user">
            <span className="online"></span>
            System Online
            <div className="avatar">A</div>
          </div>
        </header>

        <section className="welcome">
          <div>
            <span className="eyebrow">PROOF BEFORE PAYMENT</span>

            <h1>Welcome to ProofPay Nexus</h1>

            <p>
              Verify every invoice. Detect discrepancies. Pay with confidence.
            </p>
          </div>

          <ShieldCheck size={76} />
        </section>

        <section className="stats">
          <div className="stat-card">
            <ShoppingCart />
            <span>Total Purchase Orders</span>
            <strong>{orders.length}</strong>
          </div>

          <div className="stat-card">
            <FileText />
            <span>Evidence Documents</span>
            <strong>{evidenceId ? "1" : "—"}</strong>
          </div>

          <div className="stat-card">
            <AlertTriangle />
            <span>Discrepancies</span>
            <strong>{discrepancies.length}</strong>
          </div>

          <div className="stat-card">
            <Activity />
            <span>Payment Platform</span>
            <strong className="small-value">PayPal Sandbox</strong>
          </div>
        </section>

        {message && (
          <div className="notice">
            {message}
            <button onClick={() => setMessage("")}>×</button>
          </div>
        )}

        <section className="content-grid">
          {/* Purchase Orders */}
          <div className="panel">
            <div className="panel-heading">
              <div>
                <h3>Purchase Orders</h3>
                <p>Orders available for verification</p>
              </div>

              <button
                className="icon-button"
                onClick={loadOrders}
                disabled={loading}
              >
                <RefreshCw size={17} />
              </button>
            </div>

            <div className="table-wrap">
              <table>
                <thead>
                  <tr>
                    <th>PO Number</th>
                    <th>Supplier</th>
                    <th>Amount</th>
                    <th>Action</th>
                  </tr>
                </thead>

                <tbody>
                  {orders.map((order) => (
                    <tr key={order.id}>
                      <td>{order.purchaseOrderNumber}</td>
                      <td>{order.supplierName}</td>
                      <td>
                        {order.currencyCode}{" "}
                        {order.totalAmount?.toLocaleString()}
                      </td>

                      <td>
                        <button
                          className="text-button"
                          onClick={() => {
                            setSelectedOrder(order.id);
                            loadDiscrepancies(order.id);
                          }}
                        >
                          Select
                        </button>
                      </td>
                    </tr>
                  ))}

                  {orders.length === 0 && (
                    <tr>
                      <td colSpan={4}>No purchase orders found.</td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>

          {/* Evidence Verification */}
          <div className="panel">
            <div className="panel-heading">
              <div>
                <h3>Evidence Verification</h3>
                <p>Upload supplier invoice or receipt</p>
              </div>

              <Upload size={22} />
            </div>

            <label className="field-label">Purchase Order</label>

            <select
              value={selectedOrder}
              onChange={(e) => {
                const orderId = e.target.value;

                setSelectedOrder(orderId);
                loadDiscrepancies(orderId);
              }}
            >
              <option value="">Select purchase order</option>

              {orders.map((order) => (
                <option key={order.id} value={order.id}>
                  {order.purchaseOrderNumber} - {order.supplierName}
                </option>
              ))}
            </select>

            <label className="upload-area">
              <Upload size={30} />

              <strong>{file ? file.name : "Choose evidence document"}</strong>

              <span>PDF, JPG or PNG · Maximum 10 MB</span>

              <input
                type="file"
                accept=".pdf,.jpg,.jpeg,.png"
                onChange={(e) => setFile(e.target.files?.[0] ?? null)}
              />
            </label>

            <button
              className="primary-button"
              onClick={uploadEvidence}
              disabled={loading}
            >
              {loading ? "Uploading..." : "Upload Evidence"}
            </button>

            {evidenceId && (
              <div className="result-box">
                <strong>Uploaded Evidence</strong>
                <p>{evidenceId}</p>
              </div>
            )}

            <div className="ai-actions">
              <button
                className="primary-button"
                onClick={analyzeEvidence}
                disabled={loading || !evidenceId}
              >
                Analyse with Gemini AI
              </button>

              <button
                className="primary-button"
                onClick={compareInvoice}
                disabled={loading || !evidenceId}
              >
                Compare Invoice with PO
              </button>

              <button
                className="primary-button"
                onClick={assessPayment}
                disabled={loading || !selectedOrder}
              >
                Generate Payment Risk Assessment
              </button>
            </div>

            {analysis && (
              <div className="result-box">
                <h4>Gemini AI Extraction</h4>
                <pre>{JSON.stringify(analysis, null, 2)}</pre>
              </div>
            )}

            {comparison && (
              <div className="result-box">
                <h4>Invoice Comparison</h4>
                <pre>{JSON.stringify(comparison, null, 2)}</pre>
              </div>
            )}

            {assessment && (
              <div className="result-box">
                <h4>Payment Risk Assessment</h4>

                <p>
                  Risk Score:{" "}
                  <strong>{assessment.riskScore ?? "N/A"}/100</strong>
                </p>

                <p>
                  Recommendation:{" "}
                  <strong>{String(assessment.recommendation ?? "N/A")}</strong>
                </p>

                <p>{assessment.explanation}</p>
              </div>
            )}
          </div>
        </section>

        {/* Discrepancies and Payments */}
        <section className="content-grid lower">
          <div className="panel">
            <div className="panel-heading">
              <div>
                <h3>Discrepancy Review</h3>
                <p>Detected issues for selected purchase order</p>
              </div>

              <AlertTriangle />
            </div>

            {discrepancies.length === 0 ? (
              <p className="empty">Select an order to view discrepancies.</p>
            ) : (
              discrepancies.map((item) => (
                <div className="issue" key={item.id}>
                  <strong>{item.title}</strong>
                  <p>{item.description}</p>
                  <span>{String(item.severity)}</span>
                </div>
              ))
            )}
          </div>

          <div className="panel payment-panel">
            <CreditCard size={30} />

            <h3>PayPal Payment</h3>

            <p>
              Create a PayPal authorization order for an eligible, assessed
              purchase order.
            </p>

            <button
              className="primary-button"
              onClick={createPayPalOrder}
              disabled={loading || !selectedOrder}
            >
              {loading ? "Processing..." : "Continue with PayPal"}
            </button>

            <small>Sandbox environment · Human approval required</small>
          </div>
        </section>

        <footer>
          <span>ProofPay Nexus © 2026</span>
          <span>AI-powered evidence-to-payment intelligence</span>
        </footer>
      </main>
    </div>
  );
}

export default App;

"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.AgentPanel = void 0;
const vscode = __importStar(require("vscode"));
class AgentPanel {
    constructor(context, signalR) {
        this.context = context;
        this.signalR = signalR;
        this.agents = [
            { name: 'Head of Engineering', role: 'HeadOfEngineering', status: 'Running', icon: '👑' },
            { name: 'Tech Lead Backend', role: 'TechLeadBackend', status: 'Running', icon: '⚙️' },
            { name: 'Tech Lead Frontend', role: 'TechLeadFrontend', status: 'Running', icon: '🎨' },
            { name: 'Tech Lead Game', role: 'TechLeadGame', status: 'Paused', icon: '🎮' },
            { name: 'Tech Lead DevOps', role: 'TechLeadDevOps', status: 'Running', icon: '🐳' },
            { name: 'Test Lead', role: 'TestLead', status: 'Running', icon: '🧪' },
            { name: 'Chief Reviewer', role: 'ChiefReviewer', status: 'Running', icon: '👁️' },
            { name: 'Audit Lead', role: 'AuditLead', status: 'Running', icon: '📋' },
            { name: 'Architect', role: 'Architect', status: 'Running', icon: '🏗️' },
        ];
        signalR.on('AgentStatusChanged', (event, data) => {
            const agent = this.agents.find(a => a.name === data.agentName);
            if (agent) {
                agent.status = data.status;
                this._view?.webview.postMessage({ type: 'updateAgent', data });
            }
        });
    }
    resolveWebviewView(webviewView) {
        this._view = webviewView;
        webviewView.webview.options = { enableScripts: true };
        webviewView.webview.html = this.getHtml();
    }
    openInEditor() {
        const panel = vscode.window.createWebviewPanel('work-ia.dashboard', 'AI Office OS Dashboard', vscode.ViewColumn.One, { enableScripts: true });
        panel.webview.html = this.getFullDashboardHtml();
    }
    getHtml() {
        const agentCards = this.agents.map(a => `
            <div class="agent-card status-${a.status.toLowerCase()}">
                <div class="agent-icon">${a.icon}</div>
                <div class="agent-info">
                    <div class="agent-name">${a.name}</div>
                    <div class="agent-role">${a.role}</div>
                </div>
                <div class="agent-status ${a.status.toLowerCase()}">
                    <span class="status-dot"></span>
                    ${a.status}
                </div>
            </div>
        `).join('');
        return `
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body { font-family: var(--vscode-font-family); padding: 8px; }
                    .agent-card { display: flex; align-items: center; padding: 8px; margin-bottom: 4px; 
                                  background: var(--vscode-sideBar-background); border-radius: 4px; }
                    .agent-icon { font-size: 24px; margin-right: 8px; }
                    .agent-info { flex: 1; }
                    .agent-name { font-weight: 600; font-size: 13px; }
                    .agent-role { font-size: 11px; color: var(--vscode-descriptionForeground); }
                    .agent-status { font-size: 11px; display: flex; align-items: center; gap: 4px; }
                    .status-dot { width: 8px; height: 8px; border-radius: 50%; display: inline-block; }
                    .running .status-dot { background: #4CAF50; }
                    .running .status-dot { box-shadow: 0 0 4px #4CAF50; }
                    .paused .status-dot { background: #FF9800; }
                    .stopped .status-dot { background: #F44336; }
                </style>
            </head>
            <body>
                <h3>AI Office OS</h3>
                <p style="font-size: 12px; color: var(--vscode-descriptionForeground);">
                    Agent Workforce
                </p>
                ${agentCards}
            </body>
            </html>
        `;
    }
    getFullDashboardHtml() {
        return `<!DOCTYPE html><html><head><style>
            body { font-family: var(--vscode-font-family); padding: 20px; background: var(--vscode-editor-background); }
            h1 { color: var(--vscode-editor-foreground); }
            .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); gap: 12px; }
            .card { background: var(--vscode-sideBar-background); padding: 16px; border-radius: 8px; }
        </style></head><body>
            <h1>AI Office OS</h1>
            <p>Multi-agent engineering coordination platform</p>
            <div class="grid">
                ${this.agents.map(a => `<div class="card">
                    <div style="font-size: 32px;">${a.icon}</div>
                    <h3>${a.name}</h3>
                    <p>${a.role}</p>
                    <p>Status: <strong>${a.status}</strong></p>
                </div>`).join('')}
            </div>
            <script>
                const connection = new signalR.HubConnectionBuilder()
                    .withUrl('http://localhost:5000/hub/agents')
                    .build();
                connection.start();
            </script>
        </body></html>`;
    }
}
exports.AgentPanel = AgentPanel;

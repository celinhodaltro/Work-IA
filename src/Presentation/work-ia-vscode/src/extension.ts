import * as vscode from 'vscode';
import * as signalR from '@microsoft/signalr';

let connection: signalR.HubConnection;

export function activate(context: vscode.ExtensionContext) {
    const provider = new AgentViewProvider(context.extensionUri);
    context.subscriptions.push(
        vscode.window.registerWebviewViewProvider('work-ia.agentsView', provider)
    );

    connection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5000/hub/agent-states')
        .withAutomaticReconnect([0, 2000, 5000, 10000])
        .build();

    connection.on('AgentStateUpdated', (state: any) => {
        provider.updateAgent(state);
    });

    connection.start().catch(() => {});

    context.subscriptions.push(
        vscode.commands.registerCommand('work-ia.openDashboard', () => {
            vscode.env.openExternal(vscode.Uri.parse('http://localhost:5000'));
        }),
        vscode.commands.registerCommand('work-ia.hireAgent', async () => {
            const name = await vscode.window.showInputBox({ prompt: 'Agent name' });
            if (!name) return;
            const title = await vscode.window.showQuickPick(
                ['Intern', 'Junior Specialist', 'Pleno Specialist', 'Senior Specialist', 'Tech Lead', 'Architect'],
                { placeHolder: 'Select title' }
            );
            if (!title) return;
            try {
                const response = await fetch('http://localhost:5000/api/agents', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ name, title })
                });
                if (response.ok) {
                    vscode.window.showInformationMessage(`✅ ${name} hired as ${title}!`);
                }
            } catch {
                vscode.window.showErrorMessage('Failed to hire agent. Is the backend running?');
            }
        })
    );
}

class AgentViewProvider implements vscode.WebviewViewProvider {
    private _view?: vscode.WebviewView;
    private agents: Map<string, any> = new Map();

    constructor(private uri: vscode.Uri) {}

    resolveWebviewView(webviewView: vscode.WebviewView) {
        this._view = webviewView;
        webviewView.webview.options = { enableScripts: true };
        this.render();
    }

    updateAgent(state: any) {
        this.agents.set(state.id, state);
        this.render();
    }

    private render() {
        if (!this._view) return;
        const cards = Array.from(this.agents.values()).map((a: any) => `
            <div class="agent-card">
                <div class="agent-emoji">${this.getEmoji(a.emotion)}</div>
                <div class="agent-info">
                    <div class="agent-name">${a.name}</div>
                    <div class="agent-title">${a.title}</div>
                </div>
                <div class="agent-cost">${a.tokensCost || 0} tokens</div>
                <div class="status-dot ${a.status.toLowerCase()}"></div>
            </div>
        `).join('');

        this._view.webview.html = `
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body { font-family: var(--vscode-font-family); padding: 8px; }
                    .agent-card { display: flex; align-items: center; padding: 8px; margin: 4px 0;
                                  background: var(--vscode-sideBar-background); border-radius: 6px; gap: 8px; }
                    .agent-emoji { font-size: 24px; }
                    .agent-info { flex: 1; }
                    .agent-name { font-weight: 600; font-size: 13px; }
                    .agent-title { font-size: 11px; color: var(--vscode-descriptionForeground); }
                    .agent-cost { font-size: 10px; color: #888; }
                    .status-dot { width: 8px; height: 8px; border-radius: 50%; }
                    .running { background: #4CAF50; box-shadow: 0 0 4px #4CAF50; }
                    .paused { background: #FF9800; }
                </style>
            </head>
            <body>
                <h3>🏢 AI Office OS</h3>
                <p style="font-size: 11px; color: var(--vscode-descriptionForeground);">
                    ${this.agents.size} agents online
                </p>
                ${cards || '<p style="color: #888;">Waiting for agents...</p>'}
            </body>
            </html>
        `;
    }

    private getEmoji(emotion: string): string {
        const map: Record<string, string> = {
            Happy: '😊', Tired: '😰', Excited: '🎉', Stressed: '😫',
            Celebrating: '🥳', Thinking: '🤔', Neutral: '😐'
        };
        return map[emotion] || '😐';
    }
}

export function deactivate() {
    connection?.stop();
}

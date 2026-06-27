import * as vscode from 'vscode';
import { AgentPanel } from './agentPanel';
import { ActivityFeedProvider } from './activityFeedProvider';
import { SignalRService } from './signalRService';

export function activate(context: vscode.ExtensionContext) {
    const signalR = new SignalRService();
    
    const agentPanel = new AgentPanel(context, signalR);
    const activityProvider = new ActivityFeedProvider(signalR);
    
    context.subscriptions.push(
        vscode.window.registerWebviewViewProvider('work-ia.agentsView', agentPanel),
        vscode.window.registerTreeDataProvider('work-ia.activityView', activityProvider),
        vscode.commands.registerCommand('work-ia.openDashboard', () => {
            agentPanel.openInEditor();
        }),
        vscode.commands.registerCommand('work-ia.showAgents', () => {
            vscode.commands.executeCommand('work-ia.agentsView.focus');
        })
    );
    
    signalR.connect();
    
    context.subscriptions.push({
        dispose: () => signalR.disconnect()
    });
}

export function deactivate() {}

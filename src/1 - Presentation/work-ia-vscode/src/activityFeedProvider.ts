import * as vscode from 'vscode';
import { SignalRService } from './signalRService';

export class ActivityFeedProvider implements vscode.TreeDataProvider<ActivityItem> {
    private _onDidChangeTreeData = new vscode.EventEmitter<void>();
    readonly onDidChangeTreeData = this._onDidChangeTreeData.event;
    private items: ActivityItem[] = [];
    
    constructor(private signalR: SignalRService) {
        signalR.on('WorkflowEvent', (event, data) => {
            this.items.unshift(new ActivityItem(
                data.message || 'Event received',
                vscode.TreeItemCollapsibleState.None,
                data.timestamp || new Date().toISOString()
            ));
            if (this.items.length > 50) this.items = this.items.slice(0, 50);
            this._onDidChangeTreeData.fire();
        });
    }
    
    getTreeItem(element: ActivityItem): vscode.TreeItem {
        return element;
    }
    
    getChildren(): ActivityItem[] {
        return this.items;
    }
}

class ActivityItem extends vscode.TreeItem {
    constructor(
        label: string,
        collapsibleState: vscode.TreeItemCollapsibleState,
        public timestamp: string
    ) {
        super(label, collapsibleState);
        this.description = timestamp;
        this.iconPath = new vscode.ThemeIcon('bell');
        this.contextValue = 'activityItem';
    }
}

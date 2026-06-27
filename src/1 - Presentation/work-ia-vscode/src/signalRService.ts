import * as signalR from '@microsoft/signalr';

export class SignalRService {
    private connection: signalR.HubConnection;
    private listeners: Array<(event: string, data: any) => void> = [];
    
    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('http://localhost:5000/hub/agents')
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .configureLogging(signalR.LogLevel.Information)
            .build();
        
        this.connection.on('AgentStatusChanged', (data) => {
            this.notifyListeners('AgentStatusChanged', data);
        });
        
        this.connection.on('MessageReceived', (data) => {
            this.notifyListeners('MessageReceived', data);
        });
        
        this.connection.on('WorkflowEvent', (data) => {
            this.notifyListeners('WorkflowEvent', data);
        });
    }
    
    on(event: string, callback: (event: string, data: any) => void) {
        this.listeners.push(callback);
    }
    
    async connect() {
        try {
            await this.connection.start();
            console.log('SignalR connected to AI Office OS');
            this.notifyListeners('Connected', {});
        } catch (err) {
            console.error('SignalR connection failed:', err);
            setTimeout(() => this.connect(), 5000);
        }
    }
    
    async disconnect() {
        await this.connection.stop();
        this.notifyListeners('Disconnected', {});
    }
    
    private notifyListeners(event: string, data: any) {
        this.listeners.forEach(cb => cb(event, data));
    }
}

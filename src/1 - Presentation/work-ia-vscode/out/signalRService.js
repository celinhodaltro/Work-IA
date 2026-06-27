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
exports.SignalRService = void 0;
const signalR = __importStar(require("@microsoft/signalr"));
class SignalRService {
    constructor() {
        this.listeners = [];
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
    on(event, callback) {
        this.listeners.push(callback);
    }
    async connect() {
        try {
            await this.connection.start();
            console.log('SignalR connected to AI Office OS');
            this.notifyListeners('Connected', {});
        }
        catch (err) {
            console.error('SignalR connection failed:', err);
            setTimeout(() => this.connect(), 5000);
        }
    }
    async disconnect() {
        await this.connection.stop();
        this.notifyListeners('Disconnected', {});
    }
    notifyListeners(event, data) {
        this.listeners.forEach(cb => cb(event, data));
    }
}
exports.SignalRService = SignalRService;

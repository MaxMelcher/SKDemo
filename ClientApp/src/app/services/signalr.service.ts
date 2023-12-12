import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr"
import { Subject } from 'rxjs';
import { Message } from '../home/home.component';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  data: any;
  newMessageReceived: Subject<Message> = new Subject<Message>();
  pluginsReceived: Subject<string[]> = new Subject<string[]>();
  planReceived: Subject<string> = new Subject<string>();

  constructor() { }

  private hubConnection: signalR.HubConnection;

  public startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7012/plugins', {
        withCredentials: false,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err => console.log('Error while starting connection: ' + err))
  }

  public startListeners = () => {

    this.hubConnection.on('NewAnswer', (data: string) => {
      console.log(data);
      this.newMessageReceived.next({
        text: data,
        user: "bot"
      });
    });

    this.hubConnection.on('PluginsChanged', (data: string[]) => {
      this.pluginsReceived.next(data);
    });

    this.hubConnection.on('NewPlan', (data: string) => {
      this.planReceived.next(data);
    });

    this.hubConnection.on('Log', (data: string) => {
      console.log(data);
    });
  }

  public newMessage = (message: string, prompt: string, usePlugins: boolean, useMemory: boolean) => {
    return this.hubConnection.invoke('NewMessage', message, prompt, usePlugins, useMemory)
      .catch(err => console.error(err));
  }

  public newPlan = (message: string, prompt: string, usePlugins: boolean, useMemory: boolean) => {
    return this.hubConnection.invoke('NewPlan', message, prompt, usePlugins, useMemory)
      .catch(err => console.error(err));
  }

  public getPlugins = () => {
    return this.hubConnection.invoke('GetPlugins')
      .catch(err => console.error(err));
  }
}

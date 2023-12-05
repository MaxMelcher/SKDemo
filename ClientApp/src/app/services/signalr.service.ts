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
    this.hubConnection.on('pluginChanged', (data) => {
      this.data = data;
      console.log(data);
    });

    this.hubConnection.on('NewAnswer', (data: string) => {
      console.log(data);
      this.newMessageReceived.next({
        text: data,
        user: "bot"
      });
    });
  }

  public newMessage = (message: string, prompt: string) => {
    return this.hubConnection.invoke('NewMessage', message, prompt)
      .catch(err => console.error(err));
  }
}

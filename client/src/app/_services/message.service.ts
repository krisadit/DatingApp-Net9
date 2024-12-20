import { HttpClient } from '@angular/common/http';
import { EventEmitter, inject, Injectable, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { Group } from '../_models/group';
import { Message } from '../_models/message';
import { PaginatedResult } from '../_models/pagination';
import { StoredUser } from '../_models/stored-user';
import { setPaginatedResponse, setPaginationHeaders } from './pagination-helper';
import { BusyService } from './busy.service';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  private baseUrl = environment.apiUrl;
  private hubsUrl = environment.hubsUrl;
  private http = inject(HttpClient);
  private busyService = inject(BusyService);
  hubConnection?: HubConnection;
  paginatedResults = signal<PaginatedResult<Message[]> | null>(null);
  messageThread = signal<Message[]>([]);
  newMessageReceived: EventEmitter<boolean> = new EventEmitter();

  createHubConnection(user: StoredUser, otherUsername: string) {
    this.busyService.busy();
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${this.hubsUrl}message?user=${otherUsername}`, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .catch(error => console.log(error))
      .finally(() => this.busyService.idle());

    this.hubConnection.on('ReceiveMessageThread', messages => {
      this.messageThread.set(messages);
      this.newMessageReceived.emit(true);
    });

    this.hubConnection.on('NewMessage', message => {
      this.messageThread.update(messages => [...messages, message])
      this.newMessageReceived.emit(true);
    });

    this.hubConnection.on('UpdatedGroup', (group: Group) => {
      if (group.connections.some(x => x.username === otherUsername)) {
        this.messageThread.update(messages => {
          messages.forEach(message => {
            if (!message.dateRead) {
              message.dateRead = new Date(Date.now());
            }
          })
          this.newMessageReceived.emit(true);
          return messages;
        })
      }
    });
  }

  stopHubConnection() {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection.stop().catch(error => console.log(error));
    }
  }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = setPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);

    this.http.get<Message[]>(`${this.baseUrl}messages`, { observe: 'response', params })
      .subscribe({
        next: response => setPaginatedResponse(response, this.paginatedResults)
      })
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(`${this.baseUrl}messages/thread/${username}`);
  }

  sendMessage(username: string, content: string) {
    return this.hubConnection?.invoke('SendMessage', { RecipientUserName: username, Content: content });
  }

  deleteMessage(id: number) {
    return this.http.delete(`${this.baseUrl}messages/${id}`)
  }
}

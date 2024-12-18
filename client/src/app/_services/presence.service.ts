import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { StoredUser } from '../_models/stored-user';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubsUrl = environment.hubsUrl;
  private hubConnection?: HubConnection;
  private toastr = inject(ToastrService);
  onlineUsers = signal<string[]>([]);

  createHubConnection(user: StoredUser) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${this.hubsUrl}presence`, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();
    this.hubConnection.start().catch(error => console.log(error));
    this.hubConnection.on('UserIsOnline', username => this.toastr.info(`${username} is online!`));
    this.hubConnection.on('UserIsOffline', username => this.toastr.info(`${username} has left the building.`));
    this.hubConnection.on('GetOnlineUsers', usernames => this.onlineUsers.set(usernames));
  }

  stopHubConnection() {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      this.hubConnection.stop().catch(error => console.log(error));
    }
  }
}

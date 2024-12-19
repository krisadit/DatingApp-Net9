import { AfterViewChecked, Component, inject, input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { TimeagoModule } from 'ngx-timeago';
import { MessageService } from '../../_services/message.service';

@Component({
  selector: 'app-member-messages',
  imports: [FormsModule, TimeagoModule],
  templateUrl: './member-messages.component.html',
  styleUrl: './member-messages.component.css'
})
export class MemberMessagesComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('messageForm') messageForm?: NgForm;
  @ViewChild('scrollMe') scrollContainer?: any;
  messageService = inject(MessageService);
  username = input.required<string>();
  messageContent = "";
  scrollOnce = false;

  sendMessage() {
    this.messageService.sendMessage(this.username(), this.messageContent)?.then(() => {
      this.messageForm?.reset();
      this.scrollOnce = false;
    });
  }

  ngOnInit(): void {
    this.messageService.newMessageReceived.subscribe(() => {
      this.scrollOnce = false;
    })
  }

  ngAfterViewChecked(): void {
    if (!this.scrollOnce) {
      this.scrollToBottom();
    }
  }

  ngOnDestroy(): void {
    this.messageService.newMessageReceived.unsubscribe();
  }

  private scrollToBottom() {
    if (this.scrollContainer) {
      this.scrollOnce = true;
      console.log('scroll!');
      this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight + 50;
    }
  }
}

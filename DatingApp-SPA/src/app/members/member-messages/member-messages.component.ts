import { AlertifyService } from './../../_services/alertify.service';
import { AuthService } from './../../_services/auth.service';
import { UserService } from './../../_services/user.service';
import { Message } from './../../_models/Message';
import { Component, OnInit, Input } from '@angular/core';
import { tap } from 'rxjs/operators';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @Input() recipientId: number;
  messages: Message[];
  newMessage: any = {};

  constructor(private userService: UserService, private authService: AuthService, private  alertify: AlertifyService) { }

  ngOnInit() {
    this.loadMessages();
  }

  loadMessages() {
    // con pipe e tap è possibile eseguire del codice prima di passare gli oggetti ritornati dal servizio
    // al subscribe

    // il + serve per convertire in numero
    const currentUserId = +this.authService.decodedToken.nameid;
    this.userService.getMessageThread(this.authService.decodedToken.nameid, this.recipientId)
      .pipe(
        tap(messages => {
          for (let i = 0; i < messages.length; i++) {
              if (messages[i].isRead === false && messages[i].recipientId === currentUserId) {
                this.userService.markAsRead(currentUserId, messages[i].id);
              }
          }
        })
      )
      .subscribe(messages => {
        this.messages = messages;
      }, error => {
        this.alertify.error(error);
      });
  }

  sendMessage() {
    this.newMessage.recipientId = this.recipientId;
    this.userService.sendMessage(this.authService.decodedToken.nameid, this.newMessage)
      .subscribe((message: Message) => {
        // debugger; serve per fermare l'esecuzione in chrome e poter debuggare
        this.messages.unshift(message);
        this.newMessage.content = '';
      }, error => {
        this.alertify.error(error);
      });
  }

}

import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { SignalrService } from '../services/signalr.service';

export interface Message {
  text: string;
  user: string;
  plan?: boolean;
}

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {

  @ViewChild('chat', { static: false }) chat: ElementRef;

  constructor(private signalRService: SignalrService) {

    //subscribe to new messages
    this.signalRService.newMessageReceived.subscribe((message: Message) => {

      message.text = message.text.replace(/(?:\r\n|\r|\n)/g, '<br>');

      this.messages.push(message);

      setTimeout(() => {
        this.chat.nativeElement.scrollTop = this.chat.nativeElement.scrollHeight;
      }, 100);
    });

    //subscribe to plugin changes
    this.signalRService.pluginsReceived.subscribe((plugins: string[]) => {
      this.plugins = plugins;
    });
  }

  input = `What are the three rules of the Bavarian Reinheitsgebot? Provide a list.`;
  prompt = "{{$input}}";

  messages: Message[] = [];
  plugins: string[] = [];
  plans: string[] = [];
  thinking = false;

  ngOnInit() {
    //get plugins
    setTimeout(() => {
      this.signalRService.getPlugins();
    }, 1000);
  }

  sendMessage() {
    this.messages.push({ text: this.input, user: "me" });
    this.thinking = true;

    setTimeout(() => {
      this.chat.nativeElement.scrollTop = this.chat.nativeElement.scrollHeight;
    }, 100);

    this.signalRService.newMessage(this.input, this.prompt).then(() => {
      this.thinking = false;

    });
  }

  plan() {
    this.messages.push({ text: this.input, user: "me", plan: true });
    this.thinking = true;

    setTimeout(() => {
      this.chat.nativeElement.scrollTop = this.chat.nativeElement.scrollHeight;
    }, 100);

    this.signalRService.newPlan(this.input, this.prompt).then(() => {
      this.thinking = false;

    });
  }

  prompt1(){
    this.prompt = "summarize in one line, give me the TLDR: {{$input}}";
  }
  prompt2(){
    this.prompt = "Summarize and improve, write an email and finally apply Koholo {{$input}}";
  }
  prompt3(){

  }

  copy()
  {
    this.input = this.messages[this.messages.length - 1].text;
    this.input = this.input.replace(/<br>/g, '\n');
  }

}

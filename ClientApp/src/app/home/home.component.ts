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

    this.signalRService.planReceived.subscribe((plan: string) => {
      plan = plan.replace(/(?:\r\n|\r|\n)/g, '<br>');
      var m : Message = {
        text : plan,
        user : "bot",
        plan : true
      }

      this.plans.push(plan);
    });
  }

  input = `What are the three rules of the Bavarian Reinheitsgebot? Provide a list.`;
  prompt = "{{$input}}";

  messages: Message[] = [];
  plugins: string[] = [];
  plans: string[] = [];
  thinking = false;
  usePlugins = false;
  useMemory = false;

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

    this.signalRService.newMessage(this.input, this.prompt, this.usePlugins, this.useMemory).then(() => {
      this.thinking = false;

    });
  }

  plan() {
    this.messages.push({ text: this.input, user: "me", plan: true });
    this.thinking = true;
    this.plans = [];

    setTimeout(() => {
      this.chat.nativeElement.scrollTop = this.chat.nativeElement.scrollHeight;
    }, 100);

    this.signalRService.newPlan(this.input, this.prompt, this.usePlugins, this.useMemory).then(() => {
      this.thinking = false;

    });
  }

  prompt0() {
    this.prompt = "summarize in one line, give me the TLDR: {{$input}}";
  }

  prompt1() {
    this.prompt = "[SUMMARIZATION RULES]\nDONT WASTE WORDS\nUSE SHORT, CLEAR, COMPLETE SENTENCES.\nRETURN SENTENCES IN BULLET POINTS OR DASHES.\nUSE ACTIVE VOICE.\nMAXIMIZE DETAIL, MEANING\nFOCUS ON THE CONTENT\n\n[BANNED PHRASES]\nThis article\nThis document\nThis page\nThis material\n[END LIST]\n\nSummarize:\nHello how are you?\n+++++\nHello\n\nSummarize this\n{{$input}}\n+++++";
  }
  prompt2() {
    this.prompt = "Summarize, generate an email. Apply the rules of Koholo to the email:\n{{$input}}";
    this.usePlugins = true;
  }
  prompt3() {
    this.input = "What did Max learn at Microsoft? Provide a list of three relevant key learnings of 2021.";
    this.useMemory = true;
    this.prompt = "{{$input}} {{recall $input}}"
  }
  prompt4() {
    this.usePlugins = true;
    this.input = "What is my IP?";
    this.usePlugins = true;
    this.useMemory = false;
    this.prompt = "{{GetIPAddress}}. Reply with the IP address only"
  }


  copy() {
    this.input = this.messages[this.messages.length - 1].text;
    this.input = this.input.replace(/<br>/g, '\n');
  }

}

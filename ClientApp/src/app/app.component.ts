import { Component } from '@angular/core';
import { SignalrService } from './services/signalr.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'app';

  constructor(private signalRService: SignalrService) { }

  ngOnInit() {
    this.signalRService.startConnection();
    this.signalRService.addTransferChartDataListener();
  }
}

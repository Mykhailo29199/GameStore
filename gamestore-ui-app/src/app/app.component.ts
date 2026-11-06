import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FooterComponent } from './components/footer/footer.component';
import { HeaderComponent } from './components/header/header.component';
import { SiteWidgetsComponent } from './components/site-widgets/site-widgets.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent, SiteWidgetsComponent, FooterComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {}

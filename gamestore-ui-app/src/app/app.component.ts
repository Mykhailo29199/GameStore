import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FooterComponent } from './components/footer/footer.component';
import { HeaderComponent } from './components/header/header.component';
import { SiteWidgetsComponent } from './components/site-widgets/site-widgets.component';
import { LanguageService } from './core/i18n/language.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent, SiteWidgetsComponent, FooterComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {
  // The page is created again when the language changes, so it loads its data in the new language.
  protected readonly language = inject(LanguageService).current;
}

import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './components/shared/navbar/navbar.component';
import { ThemeService } from './core/services/public/theme.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  title = 'frontend';

  constructor(private themeService: ThemeService) {}

  ngOnInit() {
    // Theme service'i initialize et (constructor'da zaten yapılıyor ama emin olmak için)
    this.themeService.getTheme();
  }
}

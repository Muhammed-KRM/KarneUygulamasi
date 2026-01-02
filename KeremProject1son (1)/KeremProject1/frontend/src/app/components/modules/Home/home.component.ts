import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  features = [
    {
      title: 'Tier List Oluştur',
      description: 'Ranked, Tiered veya Fusion modları ile animelerinizi sıralayın',
      icon: '📊'
    },
    {
      title: 'MyAnimeList Entegrasyonu',
      description: 'MAL hesabınızı bağlayın ve binlerce anime arasından seçim yapın',
      icon: '🔗'
    },
    {
      title: 'Toplulukla Paylaş',
      description: 'Listelerinizi paylaşın, beğenin ve yorum yapın',
      icon: '👥'
    },
    {
      title: 'Şablonlar',
      description: 'Hazır şablonları kullanarak hızlıca liste oluşturun',
      icon: '📋'
    }
  ];
}

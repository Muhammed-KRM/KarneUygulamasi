import { Component, OnInit, HostListener, OnDestroy } from '@angular/core';
import { NavigationEnd, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/public/auth.service';
import { SocialService } from '../../../core/services/api/social.service';
import { Subscription } from 'rxjs';
import { User } from '../../../core/models/entities/user.model';
import { isResponseSuccessful } from '../../../core/utils/api-response.util';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss'
})
export class NavbarComponent implements OnInit, OnDestroy {
  isMenuOpen = false;
  isUserMenuOpen = false;
  isCreateMenuOpen = false;
  isMoreMenuOpen = false;
  unreadNotifications = 0;
  currentUser: User | null = null;
  private subscriptions = new Subscription();

  constructor(
    private router: Router,
    private authService: AuthService,
    private socialService: SocialService
  ) {}

  ngOnInit() {
    const routerSub = this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.isMenuOpen = false;
        this.isUserMenuOpen = false;
      }
    });
    this.subscriptions.add(routerSub);

    const authSub = this.authService.authState$.subscribe((user) => {
      this.currentUser = user;
      if (user) {
        this.loadNotifications();
      } else {
        this.unreadNotifications = 0;
      }
    });
    this.subscriptions.add(authSub);

    if (this.authService.isAuthenticated()) {
      this.currentUser = this.authService.user;
      this.loadNotifications();
    }
  }

  loadNotifications() {
    this.socialService.getNotifications(1, 1).subscribe({
      next: (response) => {
        if (isResponseSuccessful(response) && response.response) {
          this.unreadNotifications = response.response.unreadCount;
        }
      }
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  getUserName() {
    return this.currentUser?.username || this.authService.user?.username || '';
  }

  getUserId() {
    return this.authService.getUserId();
  }

  getUserAvatar(): string | null {
    const avatar = this.currentUser?.userImageLink || this.authService.user?.userImageLink;
    if (avatar && avatar.trim().length > 0) {
      return avatar;
    }
    return null;
  }

  isAuthenticated() {
    return this.authService.isAuthenticated();
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/']);
  }

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }
  
  toggleUserMenu() {
    this.isUserMenuOpen = !this.isUserMenuOpen;
    // Diğer menüleri kapat
    this.isCreateMenuOpen = false;
    this.isMoreMenuOpen = false;
  }

  toggleCreateMenu() {
    this.isCreateMenuOpen = !this.isCreateMenuOpen;
    this.isMoreMenuOpen = false;
    this.isUserMenuOpen = false;
  }

  toggleMoreMenu() {
    this.isMoreMenuOpen = !this.isMoreMenuOpen;
    this.isCreateMenuOpen = false;
    this.isUserMenuOpen = false;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    // Eğer tıklanan yer menülerin dışındaysa kapat
    if (!target.closest('.user-menu')) {
      this.isUserMenuOpen = false;
    }
    if (!target.closest('.create-dropdown-container')) {
      this.isCreateMenuOpen = false;
    }
    if (!target.closest('.nav-dropdown-container')) {
      this.isMoreMenuOpen = false;
    }
  }
}

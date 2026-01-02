import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/modules/Home/home.component').then(
        (m) => m.HomeComponent
      ),
    title: 'Ana Sayfa',
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./components/modules/login/login.component').then(
        (m) => m.LoginComponent
      ),
    canActivate: [guestGuard],
    title: 'Giriş Yap',
  },
  {
    path: 'signin',
    loadComponent: () =>
      import('./components/modules/singin/singin.component').then(
        (m) => m.SinginComponent
      ),
    canActivate: [guestGuard],
    title: 'Kayıt Ol',
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./components/modules/dashboard/dashboard.component').then(
        (m) => m.DashboardComponent
      ),
    canActivate: [authGuard],
    title: 'Dashboard',
  },
  {
    path: 'list/create',
    loadComponent: () =>
      import('./components/modules/list-create/list-create.component').then(
        (m) => m.ListCreateComponent
      ),
    canActivate: [authGuard],
    title: 'Liste Oluştur',
  },
  {
    path: 'list/edit/:id',
    loadComponent: () =>
      import('./components/modules/list-edit/list-edit.component').then(
        (m) => m.ListEditComponent
      ),
    canActivate: [authGuard],
    title: 'Liste Düzenle',
  },
  {
    path: 'list/view/:id',
    loadComponent: () =>
      import('./components/modules/list-view/list-view.component').then(
        (m) => m.ListViewComponent
      ),
    title: 'Liste Görüntüle',
  },
  {
    path: 'profile/settings',
    loadComponent: () =>
      import(
        './components/modules/profile-settings/profile-settings.component'
      ).then((m) => m.ProfileSettingsComponent),
    canActivate: [authGuard],
    title: 'Profil Ayarları',
  },
  {
    path: 'profile/me',
    loadComponent: () =>
      import('./components/modules/profile/profile.component').then(
        (m) => m.ProfileComponent
      ),
    canActivate: [authGuard],
    title: 'Profilim',
  },
  {
    path: 'profile/:id',
    loadComponent: () =>
      import('./components/modules/profile/profile.component').then(
        (m) => m.ProfileComponent
      ),
    title: 'Profil',
  },
  {
    path: 'notifications',
    loadComponent: () =>
      import('./components/modules/notifications/notifications.component').then(
        (m) => m.NotificationsComponent
      ),
    canActivate: [authGuard],
    title: 'Bildirimler',
  },
  {
    path: 'share/:token',
    loadComponent: () =>
      import('./components/modules/share-view/share-view.component').then(
        (m) => m.ShareViewComponent
      ),
    title: 'Paylaşılan Liste',
  },
  {
    path: 'about',
    loadComponent: () =>
      import('./components/modules/about/about.component').then(
        (m) => m.AboutComponent
      ),
    canActivate: [authGuard],
    title: 'Hakkımızda',
  },
  {
    path: 'tools',
    loadComponent: () =>
      import('./components/shared/tools/tools.component').then(
        (m) => m.ToolsComponent
      ),
    title: 'Araçlar',
  },
  {
    path: '**',
    redirectTo: '',
    pathMatch: 'full',
  },
];

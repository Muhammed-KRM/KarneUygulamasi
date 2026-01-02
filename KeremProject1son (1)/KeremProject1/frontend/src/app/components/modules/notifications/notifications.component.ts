import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SocialService } from '../../../core/services/api/social.service';
import { Notification } from '../../../core/models/entities/notification.model';
import { isResponseSuccessful } from '../../../core/utils/api-response.util';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.scss'
})
export class NotificationsComponent implements OnInit {
  notifications: Notification[] = [];
  unreadCount = 0;
  loading = false;

  constructor(private socialService: SocialService) {}

  ngOnInit() {
    this.loadNotifications();
  }

  loadNotifications() {
    this.loading = true;
    this.socialService.getNotifications(1, 50).subscribe({
      next: (response) => {
        this.loading = false;
        if (isResponseSuccessful(response) && response.response) {
          // Type assertion for notification type
          this.notifications = response.response.notifications.map(n => ({
            ...n,
            type: n.type as 'like' | 'comment' | 'follow' | 'mention'
          })) as Notification[];
          this.unreadCount = response.response.unreadCount;
        }
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  markAsRead(notificationId: number) {
    this.socialService.markNotificationAsRead(notificationId).subscribe({
      next: () => {
        const notification = this.notifications.find(n => n.id === notificationId);
        if (notification) {
          notification.isRead = true;
          this.unreadCount--;
        }
      }
    });
  }

  markAllAsRead() {
    this.socialService.markAllNotificationsAsRead().subscribe({
      next: () => {
        this.notifications.forEach(n => n.isRead = true);
        this.unreadCount = 0;
      }
    });
  }

  deleteNotification(notificationId: number) {
    this.socialService.deleteNotification(notificationId).subscribe({
      next: () => {
        this.notifications = this.notifications.filter(n => n.id !== notificationId);
      }
    });
  }
}


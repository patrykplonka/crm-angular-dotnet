import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent {
  menuItems = [
    { label: 'Dashboard', path: '/dashboard', icon: 'fas fa-home' },
    { label: 'Profil', path: '/profil', icon: 'fas fa-user' },
    { label: 'Kursy', path: '/courses', icon: 'fas fa-book' },
    { label: 'Ustawienia', path: '/settings', icon: 'fas fa-cog' }
  ];
}

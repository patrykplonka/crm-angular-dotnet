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
  isCollapsed = false;
  isMobileMenuOpen = false;

  menuItems = [
    { label: 'Panel', path: '/dashboard', icon: 'fas fa-home' },
    { label: 'Kursy', path: '/courses', icon: 'fas fa-book' },
  ];

  toggleSidebar() {
    this.isCollapsed = !this.isCollapsed;
  }

  toggleMobileMenu(event: Event) {
    event.stopPropagation();
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu() {
    if (this.isMobileMenuOpen) {
      this.isMobileMenuOpen = false;
    }
  }
}

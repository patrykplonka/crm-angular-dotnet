import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { SettingsComponent } from './components/settings/settings.component';
import { ProfilComponent } from './profil/profil.component';

@NgModule({
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule
  ],
  bootstrap: [AppComponent],
  declarations: [
    SettingsComponent,
    ProfilComponent
  ]
})
export class AppModule { }

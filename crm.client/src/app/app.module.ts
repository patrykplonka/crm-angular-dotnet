import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { RegisterComponent } from './../app/components/register/register.component';
import { LoginComponent } from './../app/components/login/login.component';

@NgModule({
  declarations: [AppComponent, RegisterComponent, LoginComponent],
  imports: [BrowserModule, HttpClientModule, FormsModule, AppRoutingModule],
  bootstrap: [AppComponent]
})
export class AppModule { }

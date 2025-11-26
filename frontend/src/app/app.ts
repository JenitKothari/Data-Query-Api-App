import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { ChangeDetectorRef, Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, CommonModule, HttpClientModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  // protected readonly title = signal('data_query_api_ui');

  apiUrl = "https://localhost:7203/api/Query/query";
  question: string = "";
  responseArea: string = "";

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  generateResponse() {
    if (!this.question) {
      return;
    }

    this.responseArea = "Loading...";

    this.http.post<any>(this.apiUrl, JSON.stringify(this.question), {
      headers: { 'Content-Type': 'application/json' }
    }).subscribe({
      next: (data) => {
        this.responseArea = JSON.stringify(data, null, 2);
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.responseArea = 'ERROR:\n' + JSON.stringify(error.error || error.message, null, 2);
        this.cdr.detectChanges();
      }
    });
  }
}

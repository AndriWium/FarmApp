import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MOVEMENT_KINDS } from '../stock-movement.model';

@Component({
  selector: 'app-movement-hub-page',
  imports: [RouterLink],
  templateUrl: './movement-hub-page.component.html',
  styleUrl: './movement-hub-page.component.scss',
})
export class MovementHubPageComponent {
  kinds = MOVEMENT_KINDS;
}

import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface ReportCard {
  path: string;
  label: string;
  description: string;
}

// Landing hub for the Reports section (doc 04, Phase 5f-1) - same "pick a card" shape as
// MovementHubPageComponent, reused here because it's already the established pattern for "several
// related but structurally different screens live under one umbrella" in this app.
const REPORT_CARDS: ReportCard[] = [
  {
    path: 'income-statement',
    label: 'Income Statement',
    description: 'Sales, cost of sales, wastage and expenses for a chosen period, month-end style.',
  },
  {
    path: 'sales-analysis',
    label: 'Sales Analysis',
    description: 'Sales by product, grade, channel, customer or day of week, with discount totals.',
  },
  {
    path: 'stock',
    label: 'Stock Reports',
    description: 'On-hand stock valuation and the opening/in/out/closing movement summary.',
  },
  {
    path: 'farming',
    label: 'Farming Report',
    description: 'Per-season cost, yield, cost/kg and margin - the "should I plant this again?" table.',
  },
  {
    path: 'cash-debtors',
    label: 'Cash & Debtors',
    description: 'Till session over/short, cash flow summary and debtors aging.',
  },
];

@Component({
  selector: 'app-reports-landing-page',
  imports: [RouterLink],
  templateUrl: './reports-landing-page.component.html',
  styleUrl: './reports-landing-page.component.scss',
})
export class ReportsLandingPageComponent {
  cards = REPORT_CARDS;
}

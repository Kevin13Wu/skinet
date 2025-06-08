import { inject, Injectable } from '@angular/core';
import { CartService } from './cart.service';
import { forkJoin, of } from 'rxjs';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root',
})
export class InitService {
  private cartService = inject(CartService);
  private accountService = inject(AccountService);

  init() {
    const cardId = localStorage.getItem('cart_id');
    const cart$ = cardId ? this.cartService.getCart(cardId) : of(null);

    return forkJoin({
      cart: cart$,
      user: this.accountService.getUserInfo(),
    });
  }
}

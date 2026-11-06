import { GameFormFields, validateGame } from './game-form.component';

const VALID: GameFormFields = {
  name: 'Vane',
  key: '',
  description: '',
  price: '24.99',
  unitInStock: '5',
  discount: '10',
  publisher: 'p1',
};

describe('validateGame', () => {
  it('accepts a complete game', () => {
    expect(validateGame(VALID)).toBe('');
  });

  it.each<[Partial<GameFormFields>, string]>([
    [{ name: '  ' }, 'Name is required.'],
    [{ publisher: '' }, 'Choose a publisher.'],
    [{ price: '-1' }, 'Price and units in stock must not be negative.'],
    [{ price: '' }, ''],
    [{ unitInStock: '1.5' }, 'Price and units in stock must not be negative.'],
    [{ discount: '101' }, 'Discount must be a whole number from 0 to 100.'],
    [{ discount: '2.5' }, 'Discount must be a whole number from 0 to 100.'],
  ])('checks %o', (change, message) => {
    expect(validateGame({ ...VALID, ...change })).toBe(message);
  });
});

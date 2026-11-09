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
    [{ name: '  ' }, 'common.nameRequired'],
    [{ publisher: '' }, 'gameForm.publisherRequired'],
    [{ price: '-1' }, 'gameForm.negative'],
    [{ price: '' }, ''],
    [{ unitInStock: '1.5' }, 'gameForm.negative'],
    [{ discount: '101' }, 'gameForm.discountRange'],
    [{ discount: '2.5' }, 'gameForm.discountRange'],
  ])('checks %o', (change, message) => {
    expect(validateGame({ ...VALID, ...change })).toBe(message);
  });
});

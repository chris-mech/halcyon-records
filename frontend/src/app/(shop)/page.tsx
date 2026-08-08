export default function Home() {
  return (
    <div className="flex flex-1 flex-col items-center justify-center gap-6 bg-background p-16 text-foreground">
      <h1 className="font-heading text-5xl font-black uppercase">
        Halcyon Records
      </h1>
      <p className="font-serif text-2xl italic">A record shop, more or less.</p>
      <div className="flex gap-4">
        <button className="rounded-none bg-primary px-6 py-3 font-medium text-primary-foreground">
          Primary
        </button>
        <button className="rounded-none bg-secondary px-6 py-3 font-medium text-secondary-foreground">
          Secondary
        </button>
        <button className="rounded-none border border-border bg-accent px-6 py-3 font-medium text-accent-foreground">
          Accent
        </button>
      </div>
    </div>
  );
}

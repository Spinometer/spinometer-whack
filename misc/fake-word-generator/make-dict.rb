def disintegrate(word)
  rx_v = %r(^[aiueoyw]+)
  rx_c = %r(^[^aiueoyw]+)

  result = []
  i = 0

  loop do
    m_v = rx_v.match(word[i..])
    if m_v
      result << m_v[0]
      i += m_v[0].size
    end

    m_c = rx_c.match(word[i..])
    if m_c
      result << m_c[0]
      i += m_c[0].size
    end

    return result if !m_v && !m_c
  end
end

def to_segments(word)
  result = []
  atoms = disintegrate(word)

  loop do
    if atoms.size < 3
      # result << atoms
      return result
    end
    result << atoms[0..2]
    atoms.shift
  end
end

def main
  words = File.readlines("/usr/share/dict/words").map { |s| s.chomp.tr("'", "").downcase }.grep(%r(^[a-z]+$)).sort.uniq.filter { |s| !s.empty? }
  segments = words.flat_map { |word| to_segments(word) }.sort.uniq
  segments.each do |s|
    puts s.join(" ")
  end
end

#p to_segments("disintegrate")
main
